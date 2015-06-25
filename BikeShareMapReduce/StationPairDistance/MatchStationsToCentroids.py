import csv
import pypyodbc
from geopy.distance import vincenty
import random
import math

def initialize_centroids(zipcode_infile) :
    # read zipcode file as initial centroids, id is auto-number, name is zipcode
    centroids = []
    centroidid = 1
    with open(zipcode_infile, 'r', newline = '') as f :
        csv_reader = csv.reader(f, delimiter=',')
        header_row = False
        col_names  = {}
        for row in csv_reader:
            if header_row == False :
                for i in range(0, len(row)):
                    col_names[row[i]] = i
                header_row = True
                continue
            centroid = {}
            centroid['name'] = row[col_names['GEOID']]
            centroid['lat'] = row[col_names['lat']]
            centroid['long'] = row[col_names['long']]
            centroid['centroidid'] = centroidid
            centroidid += 1
            centroids.append(centroid)
    return centroids

def assign_stations_to_centroids(stations, centroids) :
    # clear station list for centroids
    for centroid in centroids : 
        centroid['stations'] = []

    for station in stations :
        st_lat = station['lat']
        st_long = station['long']
        s_pt = (st_lat, st_long)
        first_dist = True
        sel_dist = -1
        sel_centroid = {}
        for centroid in centroids :
            c_lat = centroid['lat']
            c_long = centroid['long']
            c_pt = (c_lat, c_long)
            distance = vincenty(c_pt, s_pt).miles
            if first_dist == True :
                sel_dist = distance
                sel_centroid = centroid
                first_dist = False
            elif distance < sel_dist :
                sel_dist = distance
                sel_centroid = centroid
        if len(sel_centroid) > 0 :
            sel_centroid['stations'].append(station)
        else :
            print('station {} failed to find nearest centroid'.format(station['stationid']))
    # prune non-matching centroids
    centroids_to_remove = []
    empty_centroids = False
    for centroid in centroids :
        if len(centroid['stations']) == 0 :
            centroids_to_remove.append(centroid)
            empty_centroids = True
    if empty_centroids == True :
        for centroid in centroids_to_remove :
            if centroid in centroids :
                centroids.remove(centroid)
    return centroids

def record_centroids(centroid_file, station_file, centroids, iszip) :
    # called after initial assignment to zipcodes (centroid name is zipcode)
    # called after iterations for clustering (centroid name becomes (lat, long)
    # save centroids (centroidid, centroidname, lat, long)
    # save matched stations (centroidid, stationid)
    with open(station_file,'w', newline='') as f:
        station_writer = csv.writer(f, delimiter=',')
        srow = ('stationid', 'centroidid')
        station_writer.writerow(srow)
        with open(centroid_file,'w', newline='') as f:
            centroid_writer = csv.writer(f, delimiter=',')
            crow = ('centoidid', 'name', 'lat', 'long')
            centroid_writer.writerow(crow)
            for centroid in centroids :
                centoidid = centroid['centroidid']
                c_lat = centroid['lat']
                c_long = centroid['long']
                c_name = centroid['name']
                if iszip == False : 
                    c_name = '{:.7}, {:.7}'.format(c_lat, c_long) 
                crow = (centoidid, c_name, c_lat, c_long)
                centroid_writer.writerow(crow)
                stations = centroid['stations']
                for station in stations :
                    stationid = station['stationid']
                    srow = (stationid, centoidid)
                    station_writer.writerow(srow)

    return

def center_centroids(centroids) :
    # centroids include list of station details
    # update centroid lat, long to station averages
    # empty centroid station array
    # return shifted centroids
    for centroid in centroids :
        sum_lat  = 0
        sum_long = 0
        nbr_stations = 0
        for station in centroid['stations'] :
            sum_lat  += station['lat']
            sum_long += station['long']
            nbr_stations += 1
        if nbr_stations > 0 :
            centroid['lat'] = sum_lat / nbr_stations
            centroid['long'] = sum_long / nbr_stations
        centroid['stations'] = []
    return centroids

def find_centroids(centroids, stations, iterations) :
    for i in range(0,iterations) :
        centroids = center_centroids(centroids) # they will have stations assigned to them
        centroids = assign_stations_to_centroids(stations, centroids) 
    return centroids

def retrieve_stations():
    stations = []
    with pypyodbc.connect("DRIVER={SQL Server};SERVER=miranda;DATABASE=bikeshare;Trusted_Connection=true") as conx:
        curr = conx.cursor()
        # following query designed to compute unique non-zero distances
        # result is a table with no round-trips and symmetric distance between same points
        retrieved_data = curr.execute('select stationid, lat, long from stations')
        for row in retrieved_data:
            a_lat = row['lat']
            a_long = row['long']
            a_station = row['stationid']
            station = {'stationid': a_station, 'lat': a_lat, 'long': a_long}
            stations.append(station)
    return stations


if __name__ == "__main__" :

    stations = retrieve_stations()
    zipcodes = r'C:\Users\Ken\Documents\2015\bikeshare\zipcodes.csv'

    centroids = initialize_centroids(zipcodes)

    centroids = assign_stations_to_centroids(stations, centroids)

    zipcodes_file = r'C:\Users\Ken\Documents\2015\bikeshare\zip_centroids.csv'
    stationzips_file = r'C:\Users\Ken\Documents\2015\bikeshare\station_zips.csv'
    record_centroids(zipcodes_file, stationzips_file, centroids, True)

    # above process prunes out unused zips
    # reduce set further by sampling out 1/3 of these (about 30 total)
    nbr_centroids = len(centroids)
    centroids = random.sample(centroids, math.ceil(nbr_centroids/3))


    centroids = find_centroids(centroids, stations, 20)

    centroid_file = r'C:\Users\Ken\Documents\2015\bikeshare\centroids.csv'
    stationcentroids_file = r'C:\Users\Ken\Documents\2015\bikeshare\station_centroids.csv'
    record_centroids(centroid_file, stationcentroids_file, centroids, False)
