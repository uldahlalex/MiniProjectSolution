#!/usr/bin/env bash
if command -v mosquitto >/dev/null 2>&1; then
    mosquitto
else
    echo "mosquitto is not installed"
fi