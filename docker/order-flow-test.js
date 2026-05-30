import http from 'k6/http';
import { check, sleep } from 'k6';
import { createSummaryHandler } from './lib/results.js';

const baseUrl = __ENV.BASE_URL || 'http://order-service:8080';
const targetVus = Number(__ENV.TARGET_VUS || '50');
const rampUpDuration = __ENV.RAMP_UP_DURATION || '10s';
const steadyDuration = __ENV.STEADY_DURATION || '30s';
const rampDownDuration = __ENV.RAMP_DOWN_DURATION || '10s';
const iterationSleepSeconds = Number(__ENV.ITERATION_SLEEP_SECONDS || '1');

// 30 productów z seeda (30000000-0000-0000-0000-000000000001 ... 000030)
const PRODUCT_IDS = Array.from({ length: 30 }, (_, i) =>
    `30000000-0000-0000-0000-${String(i + 1).padStart(12, '0')}`
);

export const options = {
    stages: [
        { duration: rampUpDuration, target: targetVus },
        { duration: steadyDuration, target: targetVus },
        { duration: rampDownDuration, target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<5000'],
        http_req_failed: ['rate<0.01'],
    },
};

export default function () {
    const productId = PRODUCT_IDS[Math.floor(Math.random() * PRODUCT_IDS.length)];

    const payload = JSON.stringify({
        productId: productId,
        quantity: 1,
    });

    const params = {
        headers: { 'Content-Type': 'application/json' },
        tags: { benchmark: 'order-flow' },
    };

    const res = http.post(`${baseUrl}/api/orders/sync`, payload, params);

    check(res, {
        'status is 200': (r) => r.status === 200,
        'received orderId': (r) => {
            if (r.status !== 200) return false;
            const body = JSON.parse(r.body);
            return typeof body.orderId === 'string' && body.orderId.length > 0;
        },
    });

    sleep(iterationSleepSeconds);
}

export const handleSummary = createSummaryHandler('order-flow');
