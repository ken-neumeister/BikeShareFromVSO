import csv
import pypyodbc
from geopy.distance import vincenty
import random


def collect_pairs(outfile):
    with open(outfile,'w', newline='') as f:
        csv_writer = csv.writer(f, delimiter=',')
        wrow = ('a_station', 'b_station', 'distance')
        csv_writer.writerow(wrow)
        with pypyodbc.connect("DRIVER={SQL Server};SERVER=miranda;DATABASE=bikeshare;Trusted_Connection=true") as conx:
            curr = conx.cursor()
            retrieved_data = curr.execute("""
                        select a.stationid A_station, b.stationid B_station,
                               a.lat a_lat, a.long a_long,
                               b.lat b_lat, b.long b_long
                        from stations a
                        cross join stations b
                        where a.stationid < b.stationid
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
    


if __name__ == "__main__" :

    output_file = r'C:\Users\Ken\Documents\2015\bikeshare\cross_ab_distance.csv'
    collect_pairs(output_file)
