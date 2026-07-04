import http from 'k6/http';
import { check, sleep } from 'k6';
import { createSummaryHandler } from './lib/results.js';

const baseUrl = __ENV.BASE_URL || 'http://order-service:8080';
const targetVus = Number(__ENV.TARGET_VUS || '50');
const rampUpDuration = __ENV.RAMP_UP_DURATION || '10s';
const steadyDuration = __ENV.STEADY_DURATION || '30s';
const rampDownDuration = __ENV.RAMP_DOWN_DURATION || '10s';
const iterationSleepSeconds = Number(__ENV.ITERATION_SLEEP_SECONDS || '1');

export let options = {
    stages: [
        { duration: rampUpDuration, target: targetVus },
        { duration: steadyDuration, target: targetVus },
        { duration: rampDownDuration, target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<2000'],
        http_req_failed: ['rate<0.05'],
    },
};

export default function () {
    const payload = JSON.stringify({
        productId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        quantity: 1
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
        tags: {
            benchmark: 'e2e-database-test',
        },
    };

    const res = http.post(`${baseUrl}/api/orders/sync`, payload, params);

    check(res, {
        'status is 200': (r) => r.status === 200,
    });

    sleep(iterationSleepSeconds);
}

export const handleSummary = createSummaryHandler('e2e-database-test');
