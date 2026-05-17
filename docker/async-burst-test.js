import { createSummaryHandler } from './lib/results.js';
import { submitOrder, waitForTerminalOrderStatus } from './async-order-common.js';

const terminalLatencyThresholdMs = Number(__ENV.ORDER_TERMINAL_P95_MS || 8000);

export const options = {
    stages: [
        { duration: '20s', target: 20 },
        { duration: '20s', target: 20 },
        { duration: '10s', target: 200 },
        { duration: '20s', target: 200 },
        { duration: '10s', target: 400 },
        { duration: '20s', target: 400 },
        { duration: '20s', target: 0 },
    ],
    thresholds: {
        http_req_failed: ['rate<0.05'],
        http_req_duration: ['p(95)<1500'],
        order_acceptance_success: ['rate>0.95'],
        order_terminal_resolution_rate: ['rate>0.98'],
        order_status_lookup_failed: ['rate<0.02'],
        order_terminal_latency: [`p(95)<${terminalLatencyThresholdMs}`],
    },
};

export default function () {
    const submission = submitOrder();

    if (submission.accepted) {
        waitForTerminalOrderStatus(submission.orderId);
    }
}

export const handleSummary = createSummaryHandler('async-burst-test');
