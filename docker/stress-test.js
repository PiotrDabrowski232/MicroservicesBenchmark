import http from 'k6/http';
import { check, sleep } from 'k6';
import { createSummaryHandler } from './lib/results.js';

const baseUrl = __ENV.BASE_URL || 'http://order-service:8080';

export const options = {
    stages: [
        { duration: '10s', target: 20 },
        { duration: '20s', target: 20 },
        { duration: '10s', target: 200 },
        { duration: '20s', target: 200 },
        { duration: '10s', target: 0 },
    ],
};

export default function () {
    const url = `${baseUrl}/api/orders/sync`;

    const payload = JSON.stringify({
        productId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        quantity: 1,
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    const res = http.post(url, payload, params);

    check(res, {
        'status is 200': (r) => r.status === 200,
    });

    sleep(0.1);
}

export const handleSummary = createSummaryHandler('stress-test');
