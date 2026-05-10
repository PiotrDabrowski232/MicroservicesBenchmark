function sanitizeFileName(value) {
    return String(value || 'run')
        .replace(/[^a-zA-Z0-9._-]+/g, '-')
        .replace(/-+/g, '-')
        .replace(/^-|-$/g, '');
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
        `http_req_duration_avg: ${duration.avg ?? 'n/a'}`,
        `http_req_duration_p95: ${duration['p(95)'] ?? 'n/a'}`,
        `http_req_failed_rate: ${failed.rate ?? 'n/a'}`
    ].join('\n') + '\n';
}

export function createSummaryHandler(testName) {
    return function handleSummary(data) {
        const resultsDir = __ENV.RESULTS_DIR || '.';
        const runName = sanitizeFileName(__ENV.RUN_NAME || `${testName}-manual`);
        const basePath = `${resultsDir}/${runName}`;

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
            rampUpDuration: __ENV.RAMP_UP_DURATION || '',
            steadyDuration: __ENV.STEADY_DURATION || '',
            rampDownDuration: __ENV.RAMP_DOWN_DURATION || '',
            scriptPath: __ENV.SCRIPT_PATH || '',
            gitRef: __ENV.GIT_REF || ''
        };

        return {
            stdout: buildSummaryText(data, meta),
            [`${basePath}/summary.json`]: JSON.stringify(data, null, 2),
            [`${basePath}/summary.txt`]: buildSummaryText(data, meta)
        };
    };
}
