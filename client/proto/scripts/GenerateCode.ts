import { promises as fs } from "fs";
import * as path from "path";
import { execSync } from "child_process";

import { paths } from "./Paths";
import { walkDir } from "../tools/FileUtil";


export async function generateCsharpCode() {

    console.log(`生成 csharp 代码`);

    const fbs_path_list = await walkDir(paths.fbs, '.fbs');

    for (const file_path of fbs_path_list) {
        if (file_path.startsWith('ServerOnly')) {
            continue;
        }
        const full_path = path.join(paths.fbs, file_path);
        const command = `${paths.flatc} --csharp -o ${paths.csharp} ${full_path} --gen-object-api`;
        // console.log(command);
        execSync(command);
    }
}
