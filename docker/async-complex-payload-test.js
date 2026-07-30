import http from 'k6/http';
import { check, sleep } from 'k6';
import { createSummaryHandler } from './lib/results.js';


const payloadCount = Number(__ENV.PAYLOAD_COUNT || '500');
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
        http_req_duration: ['p(95)<500'],
        http_req_failed: ['rate<0.01'],
    },
};

export default function () {
    const res = http.post(`${baseUrl}/api/benchmark/async-complex-payload/${payloadCount}`, null, {
        tags: {
            benchmark: 'async-complex-payload',
            payload_count: String(payloadCount),
        },
    });

    check(res, {
        'status is 202': (r) => r.status === 202,
        'accepted expected item count': (r) => {
            if (r.status !== 202) return false;
            const body = JSON.parse(r.body);
            return body.published === payloadCount;
        },
    });

    sleep(iterationSleepSeconds);
}

export const handleSummary = createSummaryHandler('async-complex-payload-test');
