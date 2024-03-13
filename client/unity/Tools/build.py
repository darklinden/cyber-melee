#!/usr/bin/env python3
# -*- coding: utf-8 -*-
 
import argparse
import os
import subprocess

def run_cmd(cmd, logFile=None):
    print("run cmd: " + " ".join(cmd))
    process = subprocess.Popen(cmd, 
                               stdout=subprocess.DEVNULL, 
                               stderr=subprocess.DEVNULL)

    # Read output from the command in real-time
    f = None
    while True:
        if process.poll() is not None:
            break
        
        if f is None and logFile is not None:
            if os.path.exists(logFile):
                f = open(logFile, 'r')
                f.seek(0, os.SEEK_END)

        if f is not None:
            curr_position = f.tell()
            line = f.readline()
            if not line:
                f.seek(curr_position)
            else:
                print(line, end='')

    # Get the return code
    if f is not None:
        f.close()
    return_code = process.poll()
    return return_code
 
def main():

    run_cmd(['killall', 'Unity']) 
     
    parser = argparse.ArgumentParser(description=u'Unity realtime log printing build!')

    parser.add_argument('-unity', required=True, help=u'Unity executable file path')
    parser.add_argument('-project', required=True, help=u'Unity project path')
    parser.add_argument('-method', required=True, help=u'Unity method to call')

    args = parser.parse_args()

    detect_str = args.method + ' - Done'
    print("detect_str: " + detect_str)

    log_file = os.path.join(args.project, 'log.txt')

    if os.path.exists(log_file):
        os.remove(log_file)

    cmd = [args.unity, 
           '-quit', '-batchmode', '-nographics', '-force-opengl', 
           '-projectPath', args.project, 
           '-executeMethod', args.method,
           '-logFile', log_file]
     
    ret = run_cmd(cmd, log_file)

    if ret != 0:
        print("Unity build failed! ret: " + str(ret))
        exit(1)
    else:
        detect_success = False
        with open(log_file, "r") as f:
            f.seek(0)
            for line in f:
                if detect_str in line:
                    detect_success = True
         
        if detect_success:
            print("Unity build success!")
            exit(0)
        else:
            print("Unity build failed!")
            exit(1)


if __name__ == '__main__':
    main()
    