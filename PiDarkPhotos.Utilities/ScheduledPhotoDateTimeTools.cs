namespace PiDarkPhotosUtilities;

public static class ScheduledPhotoDateTimeTools
{
    public static ScheduledPhotoDateTime NextPhotoDateTime(DateTime basisDateTime, int numberOfPhotosPerDay)
    {
        return NextPhotoDateTimes(basisDateTime, numberOfPhotosPerDay).First();
    }

    public static List<ScheduledPhotoDateTime> NextPhotoDateTimes(DateTime basisDateTime, int numberOfPhotosPerDay)
    {
        var basisDateTimeTomorrow = basisDateTime.AddDays(1);

        var photoTimes = new List<(int, TimeOnly)> { (1, new TimeOnly(0, 0, 0)) };

        if (numberOfPhotosPerDay > 1)
        {
            var minutesPerSegment = 24 * 60 / (decimal)numberOfPhotosPerDay;

            for (var i = 1; i < numberOfPhotosPerDay; i++)
            {
                var minutesToAdd = minutesPerSegment * i;
                var timeToAdd = new TimeOnly(0, (int)minutesToAdd, 0);
                photoTimes.Add((i + 1, timeToAdd));
            }
        }

        var dateTimeList = new List<ScheduledPhotoDateTime>();

        foreach (var loopTimes in photoTimes)
        {
            dateTimeList.Add(new ScheduledPhotoDateTime
            {
                PhotoSeries = loopTimes.Item1,
                ScheduledTime = new DateTime(basisDateTime.Year, basisDateTime.Month, basisDateTime.Day,
                    loopTimes.Item2.Hour,
                    loopTimes.Item2.Minute, loopTimes.Item2.Second)
            });
            dateTimeList.Add(new ScheduledPhotoDateTime
            {
                PhotoSeries = loopTimes.Item1,
                ScheduledTime = new DateTime(basisDateTimeTomorrow.Year, basisDateTimeTomorrow.Month,
                    basisDateTimeTomorrow.Day, loopTimes.Item2.Hour,
                    loopTimes.Item2.Minute, loopTimes.Item2.Second)
            });
        }

        return dateTimeList.Where(x => x.ScheduledTime > basisDateTime).OrderBy(x => x.ScheduledTime).ToList();
    }
}