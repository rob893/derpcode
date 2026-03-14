FROM node:24-slim

RUN useradd -m runner
WORKDIR /home/runner
COPY run.sh ./run.sh
COPY base-driver.txt ./base-driver.js
RUN chmod +x run.sh

USER runner
CMD ["./run.sh"]
