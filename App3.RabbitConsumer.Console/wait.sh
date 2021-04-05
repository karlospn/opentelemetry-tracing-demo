#!/bin/sh

time="$1"
shift
cmd="$@"

>&2 echo "Sleeping $time seconds"
sleep $time
>&2 echo "Wait is over"
exec $cmd