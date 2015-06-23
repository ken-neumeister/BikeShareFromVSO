import csv
import pypyodbc
from geopy.distance import vincenty


if __name__ == "__main__" :
    with open(r'C:\Users\Ken\Documents\2015\bikeshare\temp_distance.csv','w', newline='') as f:
        csv_writer = csv.writer(f, delimiter=',')
        wrow = ('start_station', 'end_station', 'distance')
        csv_writer.writerow(wrow)
        with pypyodbc.connect("DRIVER={SQL Server};SERVER=miranda;DATABASE=bikeshare;Trusted_Connection=true") as conx:
            curr = conx.cursor()
            retrieved_data = curr.execute("""
select Start_station, End_station,
s.lat start_lat, s.long start_long,
e.lat end_lat, e.long end_long
from (
	select distinct Start_station, End_station
	from Trips_History
	where start_station < end_station
	) as p
inner join stations s
  on start_station = s.stationid
inner join stations e
  on end_station = e.stationid
order by start_station, end_station
    """)
            for row in retrieved_data:
                start_lat = row['start_lat']
                start_long = row['start_long']
                end_lat = row['end_lat']
                end_long = row['end_long']
                start_station = row['start_station']
                end_station = row['end_station']
                start_pt = (start_lat, start_long)
                end_pt = (end_lat, end_long)
                distance = vincenty(start_pt, end_pt).miles
                wrow = (start_station, end_station, distance)
                csv_writer.writerow(wrow)
            