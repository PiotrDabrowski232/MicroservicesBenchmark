import { sleep } from 'k6';

import { createSummaryHandler } from './lib/results.js';
import { submitOrder, waitForTerminalOrderStatus } from './async-order-common.js';

const terminalLatencyThresholdMs = Number(__ENV.ORDER_TERMINAL_P95_MS || 5000);

export const options = {
    stages: [
        { duration: '15s', target: 10 },
        { duration: '30s', target: 10 },
        { duration: '15s', target: 50 },
        { duration: '30s', target: 50 },
        { duration: '15s', target: 0 },
    ],
    thresholds: {
        http_req_failed: ['rate<0.01'],
        http_req_duration: ['p(95)<1000'],
        order_acceptance_success: ['rate>0.99'],
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

    sleep(0.2);
}

export const handleSummary = createSummaryHandler('async-accept-test');
