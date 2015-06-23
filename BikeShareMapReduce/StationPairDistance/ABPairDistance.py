import csv
import pypyodbc
from geopy.distance import vincenty


if __name__ == "__main__" :
    with open(r'C:\Users\Ken\Documents\2015\bikeshare\temp_ab_distance.csv','w', newline='') as f:
        csv_writer = csv.writer(f, delimiter=',')
        wrow = ('a_station', 'b_station', 'distance')
        csv_writer.writerow(wrow)
        with pypyodbc.connect("DRIVER={SQL Server};SERVER=miranda;DATABASE=bikeshare;Trusted_Connection=true") as conx:
            curr = conx.cursor()
            # following query designed to compute unique non-zero distances
            # result is a table with no round-trips and symmetric distance between same points
            retrieved_data = curr.execute("""
select A_station, B_station,
       a.lat a_lat, a.long a_long,
	   b.lat b_lat, b.long b_long
from (
	select distinct case when Startid < Endid then startid else endid end as A_station
	              , case when Startid < Endid then endid else startid end as B_Station
	from Trips
	where startid != endid
	) as p
inner join stations a
  on A_station = a.stationid
inner join stations b
  on B_station = b.stationid
order by A_station, B_station
    """)
            for row in retrieved_data:
                a_lat = row['a_lat']
                a_long = row['a_long']
                b_lat = row['b_lat']
                b_long = row['b_long']
                a_station = row['a_station']
                b_station = row['b_station']
                a_pt = (a_lat, a_long)
                b_pt = (b_lat, b_long)
                distance = vincenty(a_pt, b_pt).miles
                wrow = (a_station, b_station, distance)
                csv_writer.writerow(wrow)
            