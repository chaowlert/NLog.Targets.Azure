NLog.Targets.Azure
==================

NLog targets to Azure Table Storage

#### Example config

```
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
      autoReload="true">
  <extensions>
    <add assembly="NLog.Targets.Azure"/>
  </extensions>
  <targets>
    <target xsi:type="AsyncWrapper" name="azure" timeToSleepBetweenBatches="30000" batchSize="2147483647" overflowAction="Grow">
      <target xsi:type="azure" connectionName="NameOfConnectionString" tableName="LogMessage" Period="Month">
        <property name="LogLevel" value="${level}"/>
        <property name="Message" value="${message}"/>
        <property name="LoggerName" value="${logger}"/>
        <property name="ExceptionMessage" value="${exception:format=ToString}"/>
        <property name="ExceptionType" value="${exception:format=Type}"/>
      </target>
    </target>
  </targets>
  <rules>
    <logger name="*" minlevel="debug" writeTo="azure" final="true"/>
  </rules>
</nlog>
```
