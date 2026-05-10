import { check } from "k6";

export const baseUrl = __ENV.BASE_URL || "http://order-service:8080";
const invalidProductId =
    __ENV.INVALID_PRODUCT_ID || "00000000-0000-0000-0000-000000000999";
const invalidProductRatio = Number(__ENV.INVALID_PRODUCT_RATIO || 0);

const productIds = [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "6a3d4f1d-d9c2-4045-9008-345c3691cdde",
];

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
        quantity: Number(__ENV.QUANTITY || 1),
    });
}

export const params = {
    headers: {
        "Content-Type": "application/json",
    },
};

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
