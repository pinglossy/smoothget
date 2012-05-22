#! /bin/bash --
#
# Simple compilation script for smoothget on Linux with Mono.
#
# * Works with mono-gmcs 2.4.4.
# * -delaysign+ doesn't make a difference
# * Compile C# 2.0 (-langversion:ISO-2). Always targets mscorlib 2.0.
# * See http://www.mono-project.com/CSharp_Compiler for C# etc. versions.
#
# TODO: Make as many fields as possible private and readonly.
# TODO: Add concatenation of part .mkvs (why are totaltimes different?).

set -ex
gmcs -out:smoothget.exe -debug- -optimize+ -langversion:ISO-2 \
    Download.cs Mkv.cs Mp4.cs Program.cs Smooth.cs Utils.cs
