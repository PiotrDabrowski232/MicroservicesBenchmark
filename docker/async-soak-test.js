import http from 'k6/http';

import { baseUrl, buildPayload, checkAcceptedResponse, params } from './async-order-common.js';

export const options = {
    scenarios: {
        steady_async_orders: {
            executor: 'constant-arrival-rate',
            rate: Number(__ENV.RATE || 30),
            timeUnit: '1s',
            duration: __ENV.DURATION || '30m',
            preAllocatedVUs: Number(__ENV.PRE_ALLOCATED_VUS || 50),
            maxVUs: Number(__ENV.MAX_VUS || 200),
        },
    },
    thresholds: {
        http_req_failed: ['rate<0.02'],
        http_req_duration: ['p(95)<1200'],
    },
};

export default function () {
    const response = http.post(`${baseUrl}/api/orders/async`, buildPayload(), params);
    checkAcceptedResponse(response);
}
