import * as path from 'path';
import * as tl from 'azure-pipelines-task-lib/task';
import * as taskLib from 'azure-pipelines-task-lib/task';
import * as toolLib from 'azure-pipelines-tool-lib/tool';
import * as tr from 'azure-pipelines-task-lib/toolrunner';
import * as os from 'os';

const osPlat: string = os.platform();
const osArch: string = (os.arch() === 'ia32') ? 'x86' : os.arch();

async function run() {
    tl.setResourcePath(path.join(__dirname, 'task.json'));
    try {
        const versionSpec = taskLib.getInput('version', false);
        const workspacePath = taskLib.getInput('workspacePath', true);
        const connectionString = taskLib.getInput('connectionString', true);
        const targetPlatform = taskLib.getInput('targetPlatform', false);
        const autoCreateDatabase = taskLib.getInput('autoCreateDatabase', false);
        const targetVersion = taskLib.getInput('targetVersion', false);
        const tokenKeyValuePair = taskLib.getInput('tokenKeyValuePair', false);
        const delimiter = taskLib.getInput('delimiter', false);
        const additionalArguments = taskLib.getInput('additionalArguments', false);

        console.log('input_version: ' + versionSpec);
        console.log('input_workspacePath: ' + workspacePath);
        console.log('input_connectionString: ' + connectionString);
        console.log('input_targetPlatform: ' + targetPlatform);
        console.log('input_autoCreateDatabase: ' + autoCreateDatabase);
        console.log('input_targetVersion: ' + targetVersion);
        console.log('input_tokenKeyValuePair: ' + tokenKeyValuePair);
        console.log('input_delimiter: ' + delimiter);
        console.log('input_additionalArguments: ' + additionalArguments);

        console.log('var_osPlat: ' + osPlat);
        console.log('var_osArch: ' + osArch);

        //picksup the version downloaded from install task
        let versionLocation: string = '';
        if (toolLib.isExplicitVersion(versionSpec)) {
            versionLocation = versionSpec;
        } else {
            //use v0.0.0 as placeholder for latest version
            versionLocation = 'v0.0.0'
        }

        if (osPlat == 'win32') {
            var yuniqlBasePath = path.join(toolLib.findLocalTool('yuniql', versionLocation));
            console.log('var_yuniqlBasePath: ' + yuniqlBasePath);

            var yuniqlExecFilePath = path.join(yuniqlBasePath, 'yuniql.exe');
            console.log('var_yuniqlExecFilePath: ' + yuniqlExecFilePath);

            //set the plugin path
            // var pluginsPath = path.join(yuniqlBasePath, '.plugins');
            // console.log('var_pluginsPath: ' + pluginsPath);

            //builds up the arguments structure
            let yuniql = new tr.ToolRunner(yuniqlExecFilePath);
            yuniql.arg('run');

            // yuniql.arg('--plugins-path');
            // yuniql.arg(pluginsPath);

            yuniql.arg('-p');
            yuniql.arg(workspacePath);

            yuniql.arg('-c');
            yuniql.arg(connectionString);

            if (targetPlatform && targetPlatform.toLowerCase() != 'sqlserver') {
                yuniql.arg('--platform');
                yuniql.arg(targetPlatform);
            }

            if (autoCreateDatabase) {
                yuniql.arg('-a');
                yuniql.arg(autoCreateDatabase);
            }

            if (targetVersion && targetVersion.toLowerCase() != 'latest') {
                yuniql.arg('-t');
                yuniql.arg(targetVersion);
            }

            if (tokenKeyValuePair) {
                yuniql.arg('-k');
                yuniql.arg(tokenKeyValuePair);
            }

            if (delimiter) {
                yuniql.arg('--delimiter');
                yuniql.arg(delimiter);
            }

            if (additionalArguments) {
                yuniql.arg(additionalArguments);
            }

            //execute migrations with cli arguments
            let yuniqlExecOptions = {} as tr.IExecOptions;
            await yuniql.exec(yuniqlExecOptions);
        } else {
            throw new Error(`Unsupported Agent OS '${osPlat}'`);
        }
    }
    catch (err) {
        tl.setResult(tl.TaskResult.Failed, err.message);
    }
}

run();
