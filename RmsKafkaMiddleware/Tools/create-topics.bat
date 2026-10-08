@echo off
rem Kafka topics for the RMS_TEST middleware assignment
set "JAVA_HOME=C:\Program Files\Microsoft\jdk-21.0.12.101-hotspot"
set "PATH=%JAVA_HOME%\bin;%PATH%"
set "KAFKA_HEAP_OPTS=-Xmx256M"
cd /d C:\kafka\k
call bin\windows\kafka-topics.bat --bootstrap-server localhost:9092 --create --if-not-exists --topic RMS.REQUEST --partitions 1 --replication-factor 1
call bin\windows\kafka-topics.bat --bootstrap-server localhost:9092 --create --if-not-exists --topic RMS.RESPONSE --partitions 1 --replication-factor 1
call bin\windows\kafka-topics.bat --bootstrap-server localhost:9092 --list
