#!/bin/bash
pid=`cat .pid`
echo $pid
kill $pid
