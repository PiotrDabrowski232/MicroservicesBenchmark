# Microservices Communication Benchmark

## Project Description
This project is part of a master’s thesis focused on **comparing communication models in microservice architectures**.

The goal is to evaluate and compare:
- **Synchronous communication**
  - REST API
  - gRPC
- **Asynchronous communication**
  - Apache Kafka
  - RabbitMQ

using identical business logic and controlled load scenarios.

---

## Business Scenario
The system represents a **minimal e-commerce checkout flow**, consisting of:
- Order Service
- Inventory Service
- Payment Service

The scenario forces inter-service communication and allows measurement of:
- response time
- throughput
- error rate
- behavior under load

---

## Architecture
The project is implemented as a **monorepo**, containing:
- multiple independent microservices
- a load generator
- message brokers (Kafka, RabbitMQ)
- shared infrastructure and tooling

---

## Tested Communication Models
| Model        | Type           |
|-------------|----------------|
| REST        | Synchronous    |
| gRPC        | Synchronous    |
| Kafka       | Asynchronous   |
| RabbitMQ    | Asynchronous   |

---

## Repository Structure
```text
src/        - Microservices source code
tests/      - Unit and integration tests
benchmarks/ - Test scenarios and results
scripts/    - Automation scripts
