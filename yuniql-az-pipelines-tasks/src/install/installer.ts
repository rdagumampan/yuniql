import * as taskLib from 'azure-pipelines-task-lib/task';
import * as toolLib from 'azure-pipelines-tool-lib/tool';
import * as path from 'path';
import * as os from 'os';

const osPlat: string = os.platform();
const osArch: string = (os.arch() === 'ia32') ? 'x86' : os.arch();

export async function getYuniql(versionSpec: string, checkLatest: boolean) {
    try {
        console.log('var_osPlat: ' + osPlat);
        console.log('var_osArch: ' + osArch);

        // when version is explicit, we dont check the latest
        let version: string = '';
        if (toolLib.isExplicitVersion(versionSpec)) {
            checkLatest = false;
            console.log('var_isExplicitVersion: true');
            console.log('var_checkLatest: false');
        }

        // when version is explicit, check the cache
        let toolPath: string = '';
        if (!checkLatest) {
            toolPath = toolLib.findLocalTool('yuniql', versionSpec);
        }

        // when cached version doesnt exists, we download a fresh copy
        if (!toolPath) {
            //when version is explicit, use the version specified, else acquire latest version
            if (toolLib.isExplicitVersion(versionSpec)) {
                version = versionSpec;
            } else {
                //TODO: query latest match
                //TODO: Create release manifiest file
                version = "latest";
                console.log('var_version: ' + version);
            }

            //download yuniql-cli-win-x64-latest.zip
            let packageFileName: string = '';
            switch (osPlat) {
                case 'win32': packageFileName = 'yuniql-cli-win-' + osArch + '-' + version + '-full.zip'; break;
                //case 'linux': dataFileName = 'yuniql-cli-linux-' + osArch + '-' + version + '.zip'; break;
                default: throw new Error(`Unsupported Agent OS '${osPlat}'`);
            }

            const downloadBaseUrl = 'https://github.com/rdagumampan/yuniql/releases/download'
            const downloadUrl = downloadBaseUrl + '/' + version + '/' + packageFileName;
            console.log('var_downloadUrl: ' + downloadUrl);

            const temp: string = await toolLib.downloadTool(downloadUrl);
            console.log('var_temp: ' + temp);

            //extract assemblies
            const extractRoot: string = await toolLib.extractZip(temp);
            console.log('var_extractRoot: ' + extractRoot);

            //cache assemblies
            if (version != 'latest') {
                toolLib.cacheDir(extractRoot, "yuniql", version);
            } else {
                //use v0.0.0 as placeholder for latest version
                //TODO: always replace the current cached version for now
                toolLib.cleanVersion('v0.0.0');
                toolLib.cacheDir(extractRoot, "yuniql", 'v0.0.0');
            }

            //append PATH
            toolLib.prependPath(extractRoot);
        }
    }
    catch (err) {
        taskLib.setResult(taskLib.TaskResult.Failed, err.message);
    }
}