import http from 'k6/http';

import { baseUrl, buildPayload, checkAcceptedResponse, params } from './async-order-common.js';

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
    },
};

export default function () {
    const response = http.post(`${baseUrl}/api/orders/async`, buildPayload(), params);
    checkAcceptedResponse(response);
}
