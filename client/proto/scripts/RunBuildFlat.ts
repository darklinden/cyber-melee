import { relativeCopy } from "../tools/FileUtil";
import * as path from "path";
import { buildFlat } from "./BuildFlat";

async function main() {

    await buildFlat();

    // copy generated C# to unity project

    copyFiles();

    function copyFiles() {
        const root = path.resolve(__dirname, "..");
        const generated_csharp = path.join(root, 'generated/csharp');

        const unity_project = path.join(root, '../../client/unity/Assets/Plugins/Protocols');

        relativeCopy(generated_csharp, unity_project, ['.cs']);

        const generated_bytes = path.join(root, 'generated/bytes');

        const unity_bytes = path.join(root, '../../client/unity/Assets/Addrs/en-US/Configs');

        relativeCopy(generated_bytes, unity_bytes, ['.bytes']);
    }
}

main()