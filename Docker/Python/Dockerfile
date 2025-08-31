FROM python:3.13-slim

RUN useradd -m runner
WORKDIR /home/runner
COPY run.sh ./run.sh
COPY base_driver.txt ./base_driver.py
RUN chmod +x run.sh

USER runner
CMD ["./run.sh"]
