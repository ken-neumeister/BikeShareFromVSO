import csv
import pypyodbc
from geopy.distance import vincenty
#from geopy.geocoders import Nominatim
from geopy.geocoders import GoogleV3
import random
import math
import time

def initialize_centroids(zipcode_infile, new_centroidid) :
    # read zipcode file as initial centroids, id is auto-number, name is zipcode
    centroids = []
    centroidid = new_centroidid
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
            centroid['lat'] = float(row[col_names['lat']])
            centroid['long'] = float(row[col_names['long']])
            centroid['centroidid'] = centroidid
            centroidid += 1
            centroids.append(centroid)
    return centroids

def sample_centroids(centroids, factor) :
    # establish next index

    next_id = 0
    for centroid in centroids :
        centroidid = centroid['centroidid']
        if centroidid >= next_id :
            next_id = centroidid + 1

    nbr_centroids = len(centroids)
    nbr_sampled = math.ceil(nbr_centroids / factor)
    sampled_centroid_refs = random.sample(centroids,nbr_sampled)
    sampled_centroids = []
    for centroid_ref in sampled_centroid_refs:
        sampled = {}
        sampled['centroidid'] = next_id
        next_id += 1
        sampled['lat'] = centroid_ref['lat']
        sampled['long'] = centroid_ref['long']
        sampled['stations'] = []
        sampled['name'] = ''
        sampled_centroids.append(sampled)
    return sampled_centroids

def name_centroid_to_nearest_station(centroids, stations) :
    for centroid in centroids :
        c_lat = centroid['lat']
        c_long = centroid['long']
        c_pt = (c_lat, c_long)
        sel_dist = -1
        sel_station = {}
        for station in stations :
            s_lat = station['lat']
            s_long = station['long']
            s_pt = (s_lat, s_long)
            distance = vincenty(s_pt, c_pt).miles
            if sel_station == {} :
                sel_station = station
                sel_distance = distance
            elif distance < sel_distance :
                sel_station = station
                sel_distance = distance
            else :
                pass
        if sel_station != {} :
            centroid['name'] = '(+)['+ sel_station['name'] + ']'
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
        stationid = station['centroidid']
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
            if sel_centroid['centroidid'] == station['centroidid'] :
                raise RuntimeError('assigning a centroid to itself id={}'.format(station['centroidid']))
            sel_centroid['stations'].append(station)
        else :
            print('station {} failed to find nearest centroid'.format(station['centroidid']))
    # prune non-matching centroids
    centroids_to_remove = []
    empty_centroids = False
    stations_recorded = 0
    centroids_retained = 0
    for centroid in centroids :
        if len(centroid['stations']) == 0 :
            centroids_to_remove.append(centroid)
            empty_centroids = True
        else :
            #print('    {} records {} stations'.format(centroid['centroidid'],len(centroid['stations'])))
            stations_recorded += len(centroid['stations'])
            centroids_retained += 1
    if empty_centroids == True :
        for centroid in centroids_to_remove :
            if centroid in centroids :
                centroids.remove(centroid)
    #print('{} centroids now track {} stations'.format(centroids_retained,stations_recorded))
    return centroids

def record_centroids(outputpath, centroids, mapcategory, iszip, mode) :
    if iszip == True :
        map_file = outputpath + r'\station_zips_v2.csv'
        centroid_file = outputpath + r'\zip_centroids_v2.csv'
    else :
        map_file = outputpath + r'\centroid_map0.csv'
        centroid_file = outputpath + r'\centroid0.csv'
    geolocator = GoogleV3(timeout=10)
    sleep_for_google = False # will be set below if goelocator is used
    with open(map_file,mode, newline='') as f:
        station_writer = csv.writer(f, delimiter=',')
        if mode == 'w' :
            srow = ('mapcategory', 'childid', 'centroidid')
            station_writer.writerow(srow)
        with open(centroid_file,mode, newline='') as f:
            centroid_writer = csv.writer(f, delimiter=',')
            if mode == 'w' :
                if iszip == True :
                    crow = ('mapcategory','centroidid', 'lat', 'long','zipcode')
                else :
                    crow = ('mapcategory','centroidid', 'lat', 'long','name','locality')
                centroid_writer.writerow(crow)
            for centroid in centroids :
                centroidid = centroid['centroidid']
                c_lat = centroid['lat']
                c_long = centroid['long']
                c_name = centroid['name']
                if iszip == True :
                    crow = (mapcategory, centroidid, c_lat, c_long, c_name)
                else :
                    pt_name = '{:.8},{:.8}'.format(c_lat, c_long) 
                    location = geolocator.reverse(pt_name)
                    sleep_for_google = True
                    locality = location[1]
                    crow = (mapcategory, centroidid, c_lat, c_long, c_name, locality)
                centroid_writer.writerow(crow)
                stations = centroid['stations']
                for station in stations :
                    stationid = station['centroidid']
                    if stationid == centroidid :
                        raise RuntimeError('recording a centroid mapped to itself')
                    srow = (mapcategory, stationid, centroidid)
                    station_writer.writerow(srow)
                if sleep_for_google == True :
                    print('.',end='',flush=True)
                    # throttle for google map
                    time.sleep(2)
    if iszip == False :
        print()
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

def prune_centroids(centroids, limit) :
    centroids_to_remove = []
    for centroid in centroids :
        stations = centroid['stations']
        if len(stations) <= limit :
            centroids_to_remove.append(centroid)
    current_nbr_of_centroids = len(centroids)
    max_nbr_of_removals = min(current_nbr_of_centroids - 3,6) # retain at least 3 but remove no more than 6
    centroids_removed = 0
    if max_nbr_of_removals > 0 :
        for centroid in centroids_to_remove :
            centroids.remove(centroid)
            centroids_removed += 1
            if centroids_removed >= max_nbr_of_removals :
                break
    return centroids_removed # Note, other functions need refactoring, no need to pass back centroids

def find_centroids(centroids, stations, iterations) :
    for i in range(0,iterations) :
        centroids = center_centroids(centroids) # they will have stations assigned to them
        centroids = assign_stations_to_centroids(stations, centroids) 
    centroids_removed = prune_centroids(centroids, 2)
    if centroids_removed > 0 :
        print('Pruned {} centroids with 2 or fewer assigned childs, repeating iterations for {} remaining centroids'.format(centroids_removed,len(centroids))) 
        centroids = assign_stations_to_centroids(stations, centroids) 
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
        retrieved_data = curr.execute('select stationid, lat, long, name from stations')
        for row in retrieved_data:
            a_lat = row['lat']
            a_long = row['long']
            a_station = row['stationid']
            a_name = row['name']
            station = {'centroidid': a_station, 'lat': a_lat, 'long': a_long, 'name': a_name}
            stations.append(station)
    return stations


if __name__ == "__main__" :

    stations = retrieve_stations()
    # assign new centroid values above stations
    # note that this is the station master list for a quarterly import
    # used for ETL of one quarter
    # SQL will move into tables that partition this information by quarter
    new_centroidid = 0
    for station in stations :
        centroidid = station['centroidid']
        if centroidid >= new_centroidid :
            new_centroidid = centroidid + 1

    zipcodes = r'C:\Users\Ken\Documents\2015\bikeshare\zipcodes.csv'
    centroids = initialize_centroids(zipcodes,new_centroidid)
    centroids = assign_stations_to_centroids(stations, centroids)

    output_path = r'C:\Users\Ken\Documents\2015\bikeshare\tmp'
    record_centroids(output_path, centroids, 'zips', True, 'w')
    print('After initialization {} stations and {} zips'.format(len(stations),len(centroids)))

    # center centroids for locations of stations instead of zipcode centers
    centroids = find_centroids(centroids, stations, 20)
    print('  recentered {} centroids'.format(len(centroids)))

    centroids = name_centroid_to_nearest_station(centroids, stations)
    record_centroids(output_path, centroids, 'level1', False, 'w')

     # reduce set by samplign out 1/2, about 30 total
    centroids2 = sample_centroids(centroids, 2)

    centroids2 = assign_stations_to_centroids(centroids, centroids2)
    print('assigned {} centroids to {} parents'.format(len(centroids),len(centroids2)))
    centroids2 = find_centroids(centroids2, centroids, 20)
    print('  recentered {} centroids'.format(len(centroids2)))

    centroids2 = name_centroid_to_nearest_station(centroids2, stations)
    record_centroids(output_path, centroids2, 'level2', False, 'a')

    # reduce set further by sampling out 1/2 of these (about 15 total)
    centroids3 = sample_centroids(centroids2, 2)

    centroids3 = assign_stations_to_centroids(centroids2, centroids3)
    print('assigned {} centroids to {} parents'.format(len(centroids2),len(centroids3)))
    centroids3 = find_centroids(centroids3, centroids2, 20)
    print('  recentered {} centroids'.format(len(centroids3)))

    centroids3 = name_centroid_to_nearest_station(centroids3, stations)
    record_centroids(output_path, centroids3, 'level3', False, 'a')
