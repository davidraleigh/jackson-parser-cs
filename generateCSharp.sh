#!/bin/sh
#currently only running on main. Does not include tests
#-cp ./javaConversions/junit-4.8.2.jar -junitConversion 
java -jar sharpencore-0.0.1-SNAPSHOT-jar-with-dependencies.jar ./javaConversions/jackson-core-master/src/main/ @sharpen-all-options
cp -r ./javaConversions/jackson-core-master/src/main/main.net/com/fasterxml/ ./com/fasterxml/
python cleanJackson.py
