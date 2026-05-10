import http from 'k6/http';
import { check, sleep } from 'k6';
import { createSummaryHandler } from './lib/results.js';

const baseUrl = __ENV.BASE_URL || 'http://order-service:8080';

export const options = {
    stages: [
        { duration: '10s', target: 20 },
        { duration: '30s', target: 20 },
        { duration: '10s', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<100'],
        http_req_failed: ['rate<0.01'],
    },
};

export default function () {
    const res = http.get(`${baseUrl}/api/benchmark/transport-ping`);

    check(res, {
        'status is 200': (r) => r.status === 200,
        'received pong': (r) => {
            if (r.status !== 200) return false;
            const body = JSON.parse(r.body);
            return body.message === 'pong' && body.value === 1;
        },
    });

    sleep(1);
}

export const handleSummary = createSummaryHandler('transport-ping');
