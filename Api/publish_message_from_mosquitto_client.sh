#!/usr/bin/env bash
if command -v mosquitto_pub >/dev/null 2>&1; then
    mosquitto_pub -h localhost -t Messages -m '{
                                                   "eventType": "MqttClientWantsToSendMessageToRoom",
                                                   "message": "hey",
                                                   "sender": 1
                                               }'
else
    echo "mosquitto is not installed"
fi