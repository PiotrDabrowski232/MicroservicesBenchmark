function sanitizeFileName(value) {
    return String(value || 'run')
        .replace(/[^a-zA-Z0-9._-]+/g, '-')
        .replace(/-+/g, '-')
        .replace(/^-|-$/g, '');
}

function getMetricValue(metric, key) {
    if (!metric) {
        return 'n/a';
    }

    if (metric.values && metric.values[key] !== undefined) {
        return metric.values[key];
    }

    if (metric[key] !== undefined) {
        return metric[key];
    }

    return 'n/a';
}

function buildSummaryText(data, meta) {
    const metrics = data.metrics || {};
    const duration = metrics.http_req_duration || {};
    const failed = metrics.http_req_failed || {};

    return [
        `test_name: ${meta.testName}`,
        `run_name: ${meta.runName}`,
        `base_url: ${meta.baseUrl}`,
        `mode: ${meta.communicationMode}`,
        `sync_provider: ${meta.syncProvider}`,
        `async_provider: ${meta.asyncProvider}`,
        `azure_region: ${meta.azureRegion}`,
        `system_vm_name: ${meta.systemVmName}`,
        `k6_vm_name: ${meta.k6VmName}`,
        `payload_count: ${meta.payloadCount}`,
        `target_vus: ${meta.targetVus}`,
        `order_completion_timeout_ms: ${meta.orderCompletionTimeoutMs}`,
        `order_status_poll_interval_ms: ${meta.orderStatusPollIntervalMs}`,
        `http_req_duration_avg: ${getMetricValue(duration, 'avg')}`,
        `http_req_duration_p95: ${getMetricValue(duration, 'p(95)')}`,
        `http_req_failed_rate: ${getMetricValue(failed, 'rate')}`,
        `order_acceptance_latency_avg: ${getMetricValue(metrics.order_acceptance_latency, 'avg')}`,
        `order_acceptance_latency_p95: ${getMetricValue(metrics.order_acceptance_latency, 'p(95)')}`,
        `order_terminal_latency_avg: ${getMetricValue(metrics.order_terminal_latency, 'avg')}`,
        `order_terminal_latency_p95: ${getMetricValue(metrics.order_terminal_latency, 'p(95)')}`,
        `order_terminal_resolution_rate: ${getMetricValue(metrics.order_terminal_resolution_rate, 'rate')}`,
        `order_business_success_rate: ${getMetricValue(metrics.order_business_success_rate, 'rate')}`,
        `orders_completed_total: ${getMetricValue(metrics.orders_completed_total, 'count')}`,
        `orders_inventory_failed_total: ${getMetricValue(metrics.orders_inventory_failed_total, 'count')}`,
        `orders_payment_failed_total: ${getMetricValue(metrics.orders_payment_failed_total, 'count')}`,
        `orders_unresolved_total: ${getMetricValue(metrics.orders_unresolved_total, 'count')}`
    ].join('\n') + '\n';
}

export function createSummaryHandler(testName) {
    return function handleSummary(data) {
        const resultsDir = __ENV.RESULTS_DIR || '.';
        const runName = sanitizeFileName(__ENV.RUN_NAME || `${testName}-manual`);
        const useNestedRunDirectory = __ENV.RESULTS_DIR && __ENV.RUN_NAME;

        const meta = {
            testName,
            runName,
            baseUrl: __ENV.BASE_URL || 'http://order-service:8080',
            communicationMode: __ENV.COMMUNICATION_MODE || '',
            syncProvider: __ENV.SYNC_PROVIDER || '',
            asyncProvider: __ENV.ASYNC_PROVIDER || '',
            azureRegion: __ENV.AZURE_REGION || '',
            systemVmName: __ENV.SYSTEM_VM_NAME || '',
            k6VmName: __ENV.K6_VM_NAME || '',
            payloadCount: __ENV.PAYLOAD_COUNT || '',
            targetVus: __ENV.TARGET_VUS || '',
            orderCompletionTimeoutMs: __ENV.ORDER_COMPLETION_TIMEOUT_MS || '',
            orderStatusPollIntervalMs: __ENV.ORDER_STATUS_POLL_INTERVAL_MS || '',
            rampUpDuration: __ENV.RAMP_UP_DURATION || '',
            steadyDuration: __ENV.STEADY_DURATION || '',
            rampDownDuration: __ENV.RAMP_DOWN_DURATION || '',
            scriptPath: __ENV.SCRIPT_PATH || '',
            gitRef: __ENV.GIT_REF || ''
        };
        const summaryText = buildSummaryText(data, meta);

        const outputs = {
            stdout: summaryText,
        };

        if (useNestedRunDirectory) {
            outputs[`${resultsDir}/${runName}-${testName}-summary.json`] = JSON.stringify(data, null, 2);
            outputs[`${resultsDir}/${runName}-${testName}-summary.txt`] = summaryText;
            return outputs;
        }

        return {
            ...outputs,
            [`./${runName}-summary.json`]: JSON.stringify(data, null, 2),
            [`./${runName}-summary.txt`]: summaryText
        };
    };
}
