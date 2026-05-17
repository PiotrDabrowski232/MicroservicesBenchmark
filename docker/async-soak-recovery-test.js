import { createSummaryHandler } from './lib/results.js';
import { submitOrder, waitForTerminalOrderStatus } from './async-order-common.js';

const recoveryTimeoutMs = Number(__ENV.ORDER_COMPLETION_TIMEOUT_MS || 480000);
const recoveryPollIntervalMs = Number(__ENV.ORDER_STATUS_POLL_INTERVAL_MS || 1000);

export const options = {
    scenarios: {
        recovery_async_orders: {
            executor: 'constant-arrival-rate',
            rate: Number(__ENV.RATE || 15),
            timeUnit: '1s',
            duration: __ENV.DURATION || '12m',
            preAllocatedVUs: Number(__ENV.PRE_ALLOCATED_VUS || 80),
            maxVUs: Number(__ENV.MAX_VUS || 500),
            gracefulStop: __ENV.GRACEFUL_STOP || '6m',
        },
    },
    thresholds: {
        http_req_failed: ['rate<0.05'],
        order_acceptance_success: ['rate>0.95'],
        order_status_lookup_failed: ['rate<0.05'],
    },
};

export default function () {
    const submission = submitOrder();

    if (submission.accepted) {
        waitForTerminalOrderStatus(submission.orderId, {
            orderCompletionTimeoutMs: recoveryTimeoutMs,
            orderStatusPollIntervalMs: recoveryPollIntervalMs,
        });
    }
}

export const handleSummary = createSummaryHandler('async-soak-recovery-test');
