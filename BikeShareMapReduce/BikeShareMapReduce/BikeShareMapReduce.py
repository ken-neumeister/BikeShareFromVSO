import csv


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
