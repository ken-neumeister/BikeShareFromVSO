import csv
import math
import calendar
import datetime
import time


def read_stations(station_file, station_dict) :
    with open(station_file, 'r', newline = '') as f :
        csv_reader = csv.reader(f, delimiter=',')
        header_row = False
        col_names  = {}
        for row in csv_reader:
            if header_row == False :
                for i in range(0, len(row)):
                    col_names[row[i]] = i
                header_row = True
                continue
            station_dict[row[col_names['station.name']]] = row[col_names['station.id']]

def read_subscribers(subscriber_file, subscriber_dict) :
    with open(subscriber_file, 'r', newline = '') as f :
        csv_reader = csv.reader(f, delimiter=',')
        header_row = False
        col_names  = {}
        for row in csv_reader:
            if header_row == False :
                for i in range(0, len(row)):
                    col_names[row[i]] = i
                header_row = True
                continue
            subscriber_dict[row[col_names['Subscriber.type']]] = row[col_names['Subscriber.id']]

def distr_bike(start_time, end_time, quant, start_id, end_id, subscription_id, trip_cat, traffic_bikes) :
    computed_duration = end_time - start_time
    for t in range(math.floor(start_time/quant)*quant, math.floor(end_time/quant)*quant, quant) :
        t_begin = max(t, start_time)
        t_end = min(t+quant-1, end_time)
        if computed_duration == 0 :
            t_factor = 1
        else :
            t_factor = (t_end - t_begin) / computed_duration
        bike_key = "{:0},{},{},{},{}".format(t,start_id,end_id,subscription_id,trip_cat)
        if bike_key in traffic_bikes :
            traffic_bikes[bike_key] += t_factor
        else :
            traffic_bikes[bike_key] = t_factor

def record_station(record_time, station_id, subscriber_id, bike_count, station_dict):
    # start_time/quant)*quant, start_id, subscriber_id, -1, station_bikes
    station_key = '{:0},{},{}'.format(record_time, station_id, subscriber_id)
    if station_key in station_dict :
        station_dict[station_key] += bike_count
    else :
        station_dict[station_key] = bike_count

def parse_traffic_file(input_file, traffic_out_file, station_out_file, norm_traffic_file,
                       quant, max_dur, station_dict, subscriber_dict) :
    with open(norm_traffic_file, 'w', newline='') as fnorm :
        norm_writer = csv.writer(fnorm, delimiter=',')
        row = ['duration','Start.time','Start.station','End.time','End.station','Bike.number','Subscription.id']
        norm_writer.writerow(row)
        with open(input_file, 'r', newline = '') as f :
            csv_reader = csv.reader(f, delimiter=',')
            header_row = False
            linecount = 0
            col_names = {}
            station_bikes = {}
            traffic_bikes = {}
            for row in csv_reader:
                if header_row == False :
                    for i in range(0, len(row)):
                        col_names[row[i]] = i
                    header_row = True
                    linecount += 1
                    continue
                start_station = row[col_names['Start station']]
                start_id = station_dict[start_station]
                end_station = row[col_names['End station']]
                end_id = station_dict[end_station]

                subscription_type = row[col_names['Subscription Type']]
                subscription_id = subscriber_dict[subscription_type]

                start_date = row[col_names['Start date']]
                start_time = calendar.timegm(time.strptime(start_date, "%m/%d/%Y %H:%M"))
                end_date = row[col_names['End date']]
                end_time = calendar.timegm(time.strptime(end_date, "%m/%d/%Y %H:%M"))
                # revise to include subscriber
                record_station(math.floor(start_time/quant)*quant, start_id, subscription_id, -1, station_bikes)
                record_station(math.floor(end_time/quant)*quant, end_id, subscription_id, 1, station_bikes)

                duration = row[col_names['Total duration (ms)']]
                bike_number = row[col_names['Bike number']]
                # record normalized data
                row = [duration,start_time,start_id,end_time,end_id,bike_number,subscription_id]
                norm_writer.writerow(row)


                computed_duration = end_time - start_time
                if computed_duration <= max_dur :
                    distr_bike(start_time, end_time, quant, start_id, end_id, subscription_id, "nonstop", traffic_bikes)
                else :
                    distr_bike(start_time, start_time+(max_dur/2), quant, start_id, end_id, subscription_id, "begin", traffic_bikes)
                    distr_bike(end_time-(max_dur/2), end_time, quant, start_id, end_id, subscription_id, "end", traffic_bikes)
                    record_station(math.floor((start_time+2700)/quant)*quant, -1, subscription_id, 1, station_bikes)
                    record_station(math.floor((end_time-2700)/quant)*quant, -1, subscription_id, -1, station_bikes)

                linecount += 1
                #if linecount > 1000 :
                #    break

    with open(station_out_file, 'w', newline = '') as f :
        csv_writer = csv.writer(f, delimiter=',')
        row = ['Starttime', 'StationID', 'SubscriberID', 'Bikes']
        csv_writer.writerow(row)
        for station_key in station_bikes :
            [st_time, st_id, sb_id] = station_key.split(',')
            row = [st_time, st_id, sb_id, station_bikes[station_key]]
            csv_writer.writerow(row)

    with open(traffic_out_file, 'w', newline = '') as f :
        csv_writer = csv.writer(f, delimiter=',')
        row = ['Starttime', 'StartID', 'EndID', 'SubscriberID', 'TripCategory', 'Bikes']
        csv_writer.writerow(row)
        for bike_key in traffic_bikes :
            [st_time, st_id, end_id, sub_id, trip] = bike_key.split(',')
            row = [st_time, st_id, end_id, sub_id, trip, traffic_bikes[bike_key]]
            csv_writer.writerow(row)

if __name__ == "__main__" :
    station_dict = {}
    data_path = r"C:\Users\Ken\Documents\2015\bikeshare"
    station_file = data_path + r"\stations.csv"
    read_stations(station_file, station_dict)
    try:
        assert(station_dict['20th & Bell St'] == '1')
    except AssertionError:
        print ('Failed to find first entry in stations')
    subscriber_file = data_path + r"\subscribers.csv"
    subscriber_dict = {}
    read_subscribers(subscriber_file, subscriber_dict)
    try: 
        assert(subscriber_dict['Casual'] == '1')
    except AssertionError:
        print ('Failed to find first entry in subscribers')
    
    traffic_file = data_path + r"\2015-Q1-Trips-History-Data.csv"
    parsed_traffic_file = data_path + r"\2015-Q1-Parsed-Trips_v02.csv"
    parsed_station_file = data_path + r"\2015-Q1-Parsed_Stations_v02.csv"
    norm_traffic_file = data_path + r"\2015-Q1-Trips-History-Norm_v02.csv"
    quantization = 600
    max_dur = 3*3600 # more than 3 hours assume trip had a stop
    parse_traffic_file(traffic_file, parsed_traffic_file, parsed_station_file, norm_traffic_file,
                       quantization, max_dur, station_dict, subscriber_dict)


