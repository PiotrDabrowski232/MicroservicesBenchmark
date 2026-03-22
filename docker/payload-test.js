import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    stages: [
        { duration: '10s', target: 50 },
        { duration: '30s', target: 50 },
        { duration: '10s', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'],
        http_req_failed: ['rate<0.01'],
    },
};

export default function () {
    let res = http.get('http://order-service:8080/api/benchmark/data-transfer/1000');

    check(res, {
        'status is 200': (r) => r.status === 200,
        'received exactly 1000 items': (r) => {
            if (r.status !== 200) return false;
            let body = JSON.parse(r.body);
            return body.totalReceived === 1000;
        }
    });

    sleep(1);
}