# Message-Brokers-Analysis
This repository contains console applications for collecting informaions about number of messages send and received with one of three selected message brokers: RabbitMQ, Apache ActiveMQ and Kafka.

## Usage

Both message broker and applications instances are created as Docker container instances.
Every analysis scenarion is set up with its own docker-compose file

## Application types

* Producer - sends messages to broker and counts them
* Consumer - receives messages from broker and conts them

## Analysis scenarios for every message broker

* One producer - one consumer
* Multiple producers - one consumer
* One producer - multiple consumers
* Multiple producers - multiple consumers
