# PowerPosition
This is a console application that generates a CSV file with the consolidated day-ahead power position 
for a specific day. 
The application uses the PowerService.dll to retrieve the power trades for a specific day and then 
aggregates the trades per hour. The application is implemented in C# using .Net 9.

## Usage
The application can be run from the command line. The following are the command line arguments that can be used to override the appsettings:

- `--interval` or `-i`: The interval in seconds at which the application should run. The default value is 120 seconds.
- `--retryLimit` or `-rl`: The retry limit in seconds. The default value is 60 seconds.
- `--retryDelay` or `-rd`: The retry delay in milliseconds. The default value is 500 milliseconds.
- `--location` or `-l`: The location for the time zone. The default value is "Europe/Berlin".
- `--outputFolder` or `-f`: The folder path where the CSV file should be saved. The default value is "C:\\Temp\\PowerPosition".

### Example Command

```bash
PowerPosition.exe --interval 60 --retryLimit 120 --retryDelay 1000 --location "Europe/London" --outputFolder "C:\Temp"
```

The application will run every 60 seconds and will retry every 120 seconds with a delay of 1 second. The time zone will be set to
"Europe/London" and the CSV file will be saved in the "C:\Temp" folder.
The application will save files with the name `PowerPosition_YYYYMMDD_YYYYMMDDHHMM.csv` where the first date refers to the day of
the volumes (the day-ahead) while the second datetime refers to the timestamp of the extraction in UTC.

### Example Output File
`PowerPosition_20250325_202503241806.csv`
```csv
Datetime;Volume
2025-03-24T23:00:00Z;3052.30
2025-03-25T00:00:00Z;3722.36
2025-03-25T01:00:00Z;3598.88
2025-03-25T02:00:00Z;3850.88
2025-03-25T03:00:00Z;3247.55
2025-03-25T04:00:00Z;4082.92
2025-03-25T05:00:00Z;2505.22
2025-03-25T06:00:00Z;4412.52
2025-03-25T07:00:00Z;3377.33
2025-03-25T08:00:00Z;4337.37
2025-03-25T09:00:00Z;2801.43
2025-03-25T10:00:00Z;4515.08
2025-03-25T11:00:00Z;3570.09
2025-03-25T12:00:00Z;3850.45
2025-03-25T13:00:00Z;3010.38
2025-03-25T14:00:00Z;2051.86
2025-03-25T15:00:00Z;4317.63
2025-03-25T16:00:00Z;3128.69
2025-03-25T17:00:00Z;3226.57
2025-03-25T18:00:00Z;4035.57
2025-03-25T19:00:00Z;3740.04
2025-03-25T20:00:00Z;4079.53
2025-03-25T21:00:00Z;2933.56
2025-03-25T22:00:00Z;3121.11
```

## Development Challenge 
[developmentchallenge-powertrade.pdf](./assets/developmentchallenge-powertrade.pdf)
### Power trade position (C#) 
#### Overview  
Power traders need an intra-day report that provides their consolidated day-ahead power position, in 
other words, it is a report of the forecast of the total energy volume per hour required by Axpo for the next 
day. The report should generate an hourly aggregated volume and save it to a CSV file, following a 
schedule that can be configured. 
####Requirements  
1. Must be implemented as a console application using .Net Core 8 (or higher) using either C# or F#.  
2. The CSV: 
	a. It has two columns Datetime and Volume. 
	b. The first row is the header. 
	c. Semi-column is the separator. 
	d. The point is the decimal separator. 
	e. All trade positions shall be aggregated per hour (local/wall clock time). 
	f. To avoid any misunderstanding with dates or time zones, the column Datetime should be in 
UTC. 
	g. The Datetime format should follow ISO_8601. 
	h. You should mind any potential issue with Daylight Saving Time. 
3. CSV filename must follow the convention `PowerPosition_YYYYMMDD_YYYYMMDDHHMM.csv` where 
YYYYMMDD is year/month/day, e.g. 20141220 for 20 Dec 2014 and HHMM is 24hr time hour and minutes 
e.g. 1837. The first date refers to the day of the volumes (the day-ahead) while the second datetime re
fers to the timestamp of the extraction in UTC. 
4. The folder path for storing the CSV file can be either supplied on the command line or read from a config
uration file.  
5. An extract must run at a scheduled time interval; every X minutes where the actual interval X is passed on 
the command line or stored in a configuration file. This extract does not have to run exactly on the minute 
and can be within +/- 1 minute of the configured interval.  
6. It is not acceptable to miss a scheduled extract, therefore a retry mechanism should be in place. 
7. An extract must run when the application first starts and then run at the interval specified as above.  
8. The application must provide adequate logging for production support to diagnose any issues.

#### Additional Notes  
1. An assembly (.net standard 2.0) has been provided (PowerService.dll) that must be used to in
terface with the “trading system”.  
2. A single interface is provided to retrieve power trades for a specified date. Two methods are 
provided, one is a synchronous implementation (IEnumerable<PowerTrade> Get
Trades(DateTime date);) and the other is asynchronous (Task<IEnumerable<Power
Trade>> GetTradesAsync(DateTime date);). The implementation can use either of these 
methods. The class PowerService is the actual implementation of this service. 
3. The argument date refers to the reference date of the trades thus, you will need to request the 
date of the following day if you want to get the power positions of the day-ahead. Which is an 
array of PowerTrade’s. 
4. The PowerTrade class contains an array of PowerPeriods for the given day. The period 
number follow a numeric sequency starting at 1, which corresponds to the first period of the 
day.  
5. The service PowerService intends to provide trades for a specific country/location, but the 
return of PowerPeriods don’t have a specified time zone, although it internally considers a 
specifically the location, Europe\Berlin.  Your application should consider this location and 
convert to UTC for the output. We shouldn’t trust the server settings and have the location 
configured in the application. Therefore, the resultant CSV should be the same independently of 
the location of the server and its configuration. 
6. The completed solution must include all source code and be able to be compiled from source. It 
may be delivered as (in order of preference) a cloud storage link to a zip file, a link to a hosted 
source control repository, or as a zipped email attached. If you send a zipped attachment via 
email, please do not include the actual compiled executable in the zip and send a separate email 
that states that you have sent your solution via email. 
 

