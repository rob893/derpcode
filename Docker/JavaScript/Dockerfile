FROM node:22-slim

RUN useradd -m runner
WORKDIR /home/runner
COPY run.sh ./run.sh
COPY base-driver.txt ./base-driver.js
RUN chmod +x run.sh

USER runner
CMD ["./run.sh"]
