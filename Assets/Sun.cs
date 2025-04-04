using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entropedia
{

    [RequireComponent(typeof(Light))]
    [ExecuteInEditMode]
    public class Sun : MonoBehaviour
    {
        [SerializeField]
        float longitude;

        [SerializeField]
        float latitude;

        [SerializeField]
        [Range(0, 24)]
        int hour;

        [SerializeField]
        [Range(0, 60)]
        int minutes;

        DateTime time;
        Light light;

        [SerializeField]
        float timeSpeed = 1;

        [SerializeField]
        int frameSteps = 1;
        int frameStep;

        [SerializeField]
        DateTime date;

        public void SetTime(int hour, int minutes)
        {
            this.hour = hour;
            this.minutes = minutes;
            OnValidate();
        }

        public void SetLocation(float longitude, float latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }

        public void SetDate(DateTime dateTime)
        {
            this.hour = dateTime.Hour;
            this.minutes = dateTime.Minute;
            this.date = dateTime.Date;
            OnValidate();
        }

        public void SetUpdateSteps(int i)
        {
            frameSteps = i;
        }

        public void SetTimeSpeed(float speed)
        {
            timeSpeed = speed;
        }

        private void Awake()
        {

            light = GetComponent<Light>();
            time = DateTime.Now;
            hour = time.Hour;
            minutes = time.Minute;
            date = time.Date;
        }

        private void OnValidate()
        {
            time = date + new TimeSpan(hour, minutes, 0);

            Debug.Log(time);
        }

        private void Update()
        {
            time = time.AddSeconds(timeSpeed * Time.deltaTime);
            if (frameStep == 0)
            {
                SetPosition();
            }
            frameStep = (frameStep + 1) % frameSteps;


        }

        void SetPosition()
        {

            Vector3 angles = new Vector3();
            double alt;
            double azi;
            SunPosition.CalculateSunPosition(time, (double)latitude, (double)longitude, out azi, out alt);
            angles.x = (float)alt * Mathf.Rad2Deg;
            angles.y = (float)azi * Mathf.Rad2Deg;

            transform.localRotation = Quaternion.Euler(angles);
            light.intensity = Mathf.InverseLerp(-12, 0, angles.x);
        }


    }



    public static class SunPosition
    {
        private const double Deg2Rad = Math.PI / 180.0;
        private const double Rad2Deg = 180.0 / Math.PI;


        public static void CalculateSunPosition(
            DateTime dateTime, double latitude, double longitude, out double outAzimuth, out double outAltitude)
        {

            dateTime = dateTime.ToUniversalTime();


            double julianDate = 367 * dateTime.Year -
                (int)((7.0 / 4.0) * (dateTime.Year +
                (int)((dateTime.Month + 9.0) / 12.0))) +
                (int)((275.0 * dateTime.Month) / 9.0) +
                dateTime.Day - 730531.5;

            double julianCenturies = julianDate / 36525.0;


            double siderealTimeHours = 6.6974 + 2400.0513 * julianCenturies;

            double siderealTimeUT = siderealTimeHours +
                (366.2422 / 365.2422) * (double)dateTime.TimeOfDay.TotalHours;

            double siderealTime = siderealTimeUT * 15 + longitude;


            julianDate += (double)dateTime.TimeOfDay.TotalHours / 24.0;
            julianCenturies = julianDate / 36525.0;


            double meanLongitude = CorrectAngle(Deg2Rad *
                (280.466 + 36000.77 * julianCenturies));

            double meanAnomaly = CorrectAngle(Deg2Rad *
                (357.529 + 35999.05 * julianCenturies));

            double equationOfCenter = Deg2Rad * ((1.915 - 0.005 * julianCenturies) *
                Math.Sin(meanAnomaly) + 0.02 * Math.Sin(2 * meanAnomaly));

            double elipticalLongitude =
                CorrectAngle(meanLongitude + equationOfCenter);

            double obliquity = (23.439 - 0.013 * julianCenturies) * Deg2Rad;


            double rightAscension = Math.Atan2(
                Math.Cos(obliquity) * Math.Sin(elipticalLongitude),
                Math.Cos(elipticalLongitude));

            double declination = Math.Asin(
                Math.Sin(rightAscension) * Math.Sin(obliquity));


            double hourAngle = CorrectAngle(siderealTime * Deg2Rad) - rightAscension;

            if (hourAngle > Math.PI)
            {
                hourAngle -= 2 * Math.PI;
            }

            double altitude = Math.Asin(Math.Sin(latitude * Deg2Rad) *
                Math.Sin(declination) + Math.Cos(latitude * Deg2Rad) *
                Math.Cos(declination) * Math.Cos(hourAngle));


            double aziNom = -Math.Sin(hourAngle);
            double aziDenom =
                Math.Tan(declination) * Math.Cos(latitude * Deg2Rad) -
                Math.Sin(latitude * Deg2Rad) * Math.Cos(hourAngle);

            double azimuth = Math.Atan(aziNom / aziDenom);

            if (aziDenom < 0)
            {
                azimuth += Math.PI;
            }
            else if (aziNom < 0)
            {
                azimuth += 2 * Math.PI;
            }

            outAltitude = altitude;
            outAzimuth = azimuth;
        }


        private static double CorrectAngle(double angleInRadians)
        {
            if (angleInRadians < 0)
            {
                return 2 * Math.PI - (Math.Abs(angleInRadians) % (2 * Math.PI));
            }
            else if (angleInRadians > 2 * Math.PI)
            {
                return angleInRadians % (2 * Math.PI);
            }
            else
            {
                return angleInRadians;
            }
        }
    }

}