import http from 'k6/http';
import { sleep } from 'k6';

import { baseUrl, buildPayload, checkAcceptedResponse, params } from './async-order-common.js';

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
    },
};

export default function () {
    const response = http.post(`${baseUrl}/api/orders/async`, buildPayload(), params);
    checkAcceptedResponse(response);
    sleep(0.2);
}
