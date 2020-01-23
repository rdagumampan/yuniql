import * as taskLib from 'azure-pipelines-task-lib/task';
//import * as toolLib from 'vsts-task-tool-lib/tool';
import * as installer from './installer';
// import * as proxyutil from './proxyutil';

async function run() {
    try {
        //get target version
        const version = taskLib.getInput('version', false);
        console.log('input_version: ' + version);
        if (version) {
            await installer.getYuniql(version, true);
        }
    }
    catch (error) {
        taskLib.setResult(taskLib.TaskResult.Failed, error.message);
    }
}

run()