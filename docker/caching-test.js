import http from 'k6/http';
import { check, sleep } from 'k6';
import { createSummaryHandler } from './lib/results.js';

const payloadCount = Number(__ENV.PAYLOAD_COUNT || '500'); // Duży ładunek
const baseUrl = __ENV.BASE_URL || 'http://order-service:8080';
const targetVus = Number(__ENV.TARGET_VUS || '50');
const rampUpDuration = __ENV.RAMP_UP_DURATION || '10s';
const steadyDuration = __ENV.STEADY_DURATION || '30s';
const rampDownDuration = __ENV.RAMP_DOWN_DURATION || '10s';
const iterationSleepSeconds = Number(__ENV.ITERATION_SLEEP_SECONDS || '0'); // Brak sleepa, chcemy maksymalnie docisnąć cache!

export let options = {
    stages: [
        { duration: rampUpDuration, target: targetVus },
        { duration: steadyDuration, target: targetVus },
        { duration: rampDownDuration, target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<1000'],
        http_req_failed: ['rate<0.01'],
    },
};

export default function () {
    const res = http.get(`${baseUrl}/api/benchmark/complex-data-transfer/${payloadCount}`, {
        tags: {
            benchmark: 'edge-caching-test',
            payload_count: String(payloadCount),
        },
    });

    check(res, {
        'status is 200': (r) => r.status === 200,
    });

    sleep(iterationSleepSeconds);
}

export const handleSummary = createSummaryHandler('caching-test');
