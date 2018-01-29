KttLogsService is a windows service sync between clock HW and KTT.
It retrieves data from clock hardware and sends it to KTT\att_sync.php.

#installation steps:

1. rename Communication Protocol SDK(32Bit Ver6.2.4.1).sdk suffix to .zip and unzip folder
2. copy folder to C:\WINDOWS\sysWOW64
3. register zkemkeeper dll (regsvr32 zkemkeeper.dll)
4. install service: Run setup.exe kttLogSetup.msi as admin

#configuration:

1. from - date time from when data should be syncronized (i.e. 15/09/2017_00:00)
2. ktturi - KTT server address (i.e. http://www.korentec.co.il/kttdebug)
3. attip - clock HW machine ip (i.e. 192.168.1.7)
4. attport - clock HW machine port (i.e. 4370)
5. Command - sync
6. lastSync - date time value for last sync time

