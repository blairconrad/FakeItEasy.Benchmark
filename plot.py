#!/usr/bin/env python

"""A simple python script template.

"""

from __future__ import print_function
import os
import sys
import argparse
import matplotlib.pyplot as plot
import csv
import re


def clean_group(group):
    return group.strip("'")


def to_ns(string_value):
    string_value = string_value.replace(",", "")
    if string_value.endswith(" ns"):
        return float(string_value[:-3])
    elif string_value.endswith(" us"):
        return float(string_value[:-3]) * 1000


class SeriesSortKeySource:
    def __init__(self, preferred_series_order):
        if preferred_series_order:
            self.preferred_series_order = preferred_series_order
        else:
            self.preferred_series_order = []

    def __call__(self, series):
        try:
            return self.preferred_series_order.index(series)
        except:
            return series


def main(arguments):

    parser = argparse.ArgumentParser(
        description=__doc__,
        formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("infile", help="Input file",
                        type=argparse.FileType("r"))
    parser.add_argument("series", nargs="*")
    parser.add_argument("--group-key", help="Column key to form groups out of",
                        type=str, default="Method")
    parser.add_argument("--series-key", help="Column key to form series out of",
                        type=str, default="Job")

    args = parser.parse_args(arguments)

    reader = csv.DictReader(args.infile)

    group_key = args.group_key
    series_key = args.series_key

    groups = set()
    serieses = set()
    means = dict()
    errors = dict()
    for row in reader:
        group = row[group_key]
        series = row[series_key]

        groups.add(group)
        serieses.add(series)

        mean = to_ns(row["Mean"])
        means[(group, series)] = float(mean)

        error = to_ns(row["Error"])
        errors[(group, series)] = float(error)

    series_key_source = SeriesSortKeySource(args.series)

    groups = sorted(list(groups), key=clean_group)
    serieses = sorted(list(serieses), key=series_key_source)

    bar_width = 1.0 / (len(serieses) + 1)

    figure, axes = plot.subplots()
    figure.set_size_inches(10, 5)

    series_number = 0
    for series in serieses:
        series_means = [means[(group, series)] for group in groups]
        series_errors = [errors[(group, series)] for group in groups]

        offsets = [offset - series_number *
                   bar_width for offset in range(len(groups))]
        axes.barh(offsets, series_means, bar_width,
                  xerr=series_errors, label=series)

        series_number += 1

    ytick_offset = -(len(serieses) - 1) * bar_width / 2.0

    axes.set_xlabel(u"Time (nsec)")
    axes.set_yticks([o + ytick_offset for o in range(len(groups))])
    axes.set_yticklabels([clean_group(group) for group in groups])
    axes.legend()

    figure.tight_layout()

    plot.savefig(os.path.splitext(args.infile.name)[0] + ".png")


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
