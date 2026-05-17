import http from "k6/http";
import { check, sleep } from "k6";
import { Counter, Rate, Trend } from "k6/metrics";

export const baseUrl = __ENV.BASE_URL || "http://order-service:8080";

const invalidProductId =
    __ENV.INVALID_PRODUCT_ID || "00000000-0000-0000-0000-000000000999";
const invalidProductRatio = Number(__ENV.INVALID_PRODUCT_RATIO || 0);
const quantity = Number(__ENV.QUANTITY || 1);
const orderCompletionTimeoutMs = Number(__ENV.ORDER_COMPLETION_TIMEOUT_MS || 15000);
const orderStatusPollIntervalMs = Number(__ENV.ORDER_STATUS_POLL_INTERVAL_MS || 500);

const terminalStatuses = new Set(["Completed", "InventoryFailed", "PaymentFailed"]);
const defaultProductIds = [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "6a3d4f1d-d9c2-4045-9008-345c3691cdde",
];

function getProductIds() {
    if (!__ENV.PRODUCT_IDS) {
        return defaultProductIds;
    }

    const parsedProductIds = __ENV.PRODUCT_IDS
        .split(",")
        .map((productId) => productId.trim())
        .filter(Boolean);

    return parsedProductIds.length > 0 ? parsedProductIds : defaultProductIds;
}

const productIds = getProductIds();

export const acceptanceLatency = new Trend("order_acceptance_latency", true);
export const terminalLatency = new Trend("order_terminal_latency", true);
export const statusLookupLatency = new Trend("order_status_lookup_latency", true);

export const acceptanceSuccessRate = new Rate("order_acceptance_success");
export const terminalResolutionRate = new Rate("order_terminal_resolution_rate");
export const statusLookupFailureRate = new Rate("order_status_lookup_failed");
export const businessSuccessRate = new Rate("order_business_success_rate");

export const acceptedOrders = new Counter("orders_accepted_total");
export const completedOrders = new Counter("orders_completed_total");
export const inventoryFailedOrders = new Counter("orders_inventory_failed_total");
export const paymentFailedOrders = new Counter("orders_payment_failed_total");
export const unresolvedOrders = new Counter("orders_unresolved_total");

export const params = {
    headers: {
        "Content-Type": "application/json",
    },
    tags: {
        name: "POST /api/orders/async",
        endpoint: "create-order-async",
    },
};

export function randomProductId() {
    if (__ENV.PRODUCT_ID) {
        return __ENV.PRODUCT_ID;
    }

    if (invalidProductRatio > 0 && Math.random() < invalidProductRatio) {
        return invalidProductId;
    }

    const index = Math.floor(Math.random() * productIds.length);
    return productIds[index];
}

export function buildPayload() {
    return JSON.stringify({
        productId: randomProductId(),
        quantity,
    });
}

export function checkAcceptedResponse(response) {
    return check(response, {
        "status is 200": (r) => r.status === 200,
        "body has order id and status": (r) => {
            if (r.status !== 200) {
                return false;
            }

            const body = r.json();
            return !!body.id && typeof body.status === "string";
        },
    });
}

export function submitOrder() {
    const response = http.post(`${baseUrl}/api/orders/async`, buildPayload(), params);
    acceptanceLatency.add(response.timings.duration);

    const accepted = checkAcceptedResponse(response);
    acceptanceSuccessRate.add(accepted);

    if (!accepted) {
        return { accepted: false, response };
    }

    const body = response.json();
    acceptedOrders.add(1);

    return {
        accepted: true,
        response,
        orderId: body.id,
        acceptedStatus: body.status,
    };
}

export function waitForTerminalOrderStatus(orderId, overrides = {}) {
    const startTime = Date.now();
    const completionTimeoutMs = Number(
        overrides.orderCompletionTimeoutMs || orderCompletionTimeoutMs
    );
    const statusPollIntervalMs = Number(
        overrides.orderStatusPollIntervalMs || orderStatusPollIntervalMs
    );
    const deadline = startTime + completionTimeoutMs;

    while (Date.now() <= deadline) {
        const response = http.get(`${baseUrl}/api/orders/${orderId}`, {
            tags: {
                name: "GET /api/orders/:id",
                endpoint: "get-order-status",
            },
        });

        statusLookupLatency.add(response.timings.duration);

        if (response.status === 200) {
            statusLookupFailureRate.add(false);

            const body = response.json();
            const currentStatus = body.status;

            if (terminalStatuses.has(currentStatus)) {
                const elapsedMs = Date.now() - startTime;

                terminalLatency.add(elapsedMs);
                terminalResolutionRate.add(true);

                if (currentStatus === "Completed") {
                    completedOrders.add(1);
                    businessSuccessRate.add(true);
                } else if (currentStatus === "InventoryFailed") {
                    inventoryFailedOrders.add(1);
                    businessSuccessRate.add(false);
                } else if (currentStatus === "PaymentFailed") {
                    paymentFailedOrders.add(1);
                    businessSuccessRate.add(false);
                }

                return {
                    resolved: true,
                    status: currentStatus,
                    elapsedMs,
                    response,
                };
            }
        } else {
            statusLookupFailureRate.add(true);
        }

        sleep(statusPollIntervalMs / 1000);
    }

    terminalResolutionRate.add(false);
    unresolvedOrders.add(1);

    return {
        resolved: false,
        status: "Timeout",
        elapsedMs: Date.now() - startTime,
    };
}
