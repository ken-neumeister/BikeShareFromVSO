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

def parse_traffic_file(input_file, traffic_out_file, station_out_file, quant, station_dict, subscriber_dict) :
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
            station_key = '{:0},{}'.format(math.floor(start_time/quant)*quant, start_id)
            if station_key in station_bikes :
                station_bikes[station_key] -= 1
            else :
                station_bikes[station_key] = -1

            station_key = '{:0},{}'.format(math.floor(end_time/quant)*quant, end_id)
            if station_key in station_bikes :
                station_bikes[station_key] += 1
            else :
                station_bikes[station_key] = 1

            duration = row[col_names['Total duration (ms)']]
            if duration.isnumeric == False or int(duration) > 5400000 :
                # Several extremely long trips observed probably involving stop-overs
                # focus on this study is for station-to-station trips
                continue

            computed_duration = end_time - start_time
            for t in range(math.floor(start_time/quant)*quant, math.floor(end_time/quant)*quant, quant) :
                t_begin = max(t, start_time)
                t_end = min(t+quant-1, end_time)
                if computed_duration == 0 :
                    t_factor = 1
                else :
                    t_factor = (t_end - t_begin) / computed_duration
                bike_key = "{:0},{},{},{}".format(t,start_id,end_id,subscription_id)
                if bike_key in traffic_bikes :
                    traffic_bikes[bike_key] += t_factor
                else :
                    traffic_bikes[bike_key] = t_factor

            linecount += 1
            #if linecount > 1000 :
            #    break

    with open(station_out_file, 'w', newline = '') as f :
        csv_writer = csv.writer(f, delimiter=',')
        row = ['Starttime', 'StartID', 'Bikes']
        csv_writer.writerow(row)
        for station_key in station_bikes :
            [st_time, st_id] = station_key.split(',')
            row = [st_time, st_id, station_bikes[station_key]]
            csv_writer.writerow(row) 

    with open(traffic_out_file, 'w', newline = '') as f :
        csv_writer = csv.writer(f, delimiter=',')
        row = ['Starttime', 'StartID', 'EndID', 'SubscriberID', 'Bikes']
        csv_writer.writerow(row)
        for bike_key in traffic_bikes :
            [st_time, st_id, end_id, sub_id] = bike_key.split(',')
            row = [st_time, st_id, end_id, sub_id, traffic_bikes[bike_key]]
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
    parsed_traffic_file = data_path + r"\2015-Q1-Parsed-Trips.csv"
    parsed_station_file = data_path + r"\2015-Q1-Parsed_Stations.csv"
    quantization = 600
    parse_traffic_file(traffic_file, parsed_traffic_file, parsed_station_file, quantization, station_dict, subscriber_dict)


