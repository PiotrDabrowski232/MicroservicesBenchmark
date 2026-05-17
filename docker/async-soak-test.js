import { createSummaryHandler } from './lib/results.js';
import { submitOrder, waitForTerminalOrderStatus } from './async-order-common.js';

const terminalLatencyThresholdMs = Number(__ENV.ORDER_TERMINAL_P95_MS || 7000);

export const options = {
    scenarios: {
        steady_async_orders: {
            executor: 'constant-arrival-rate',
            rate: Number(__ENV.RATE || 30),
            timeUnit: '1s',
            duration: __ENV.DURATION || '30m',
            preAllocatedVUs: Number(__ENV.PRE_ALLOCATED_VUS || 50),
            maxVUs: Number(__ENV.MAX_VUS || 200),
        },
    },
    thresholds: {
        http_req_failed: ['rate<0.02'],
        http_req_duration: ['p(95)<1200'],
        order_acceptance_success: ['rate>0.98'],
        order_terminal_resolution_rate: ['rate>0.99'],
        order_status_lookup_failed: ['rate<0.01'],
        order_terminal_latency: [`p(95)<${terminalLatencyThresholdMs}`],
    },
};

export default function () {
    const submission = submitOrder();

    if (submission.accepted) {
        waitForTerminalOrderStatus(submission.orderId);
    }
}

export const handleSummary = createSummaryHandler('async-soak-test');
