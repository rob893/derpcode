FROM rust:1.87-slim

RUN useradd -m runner
WORKDIR /home/runner

RUN mkdir src
RUN echo "fn main() {}" > src/main.rs
COPY . .
RUN cargo build --release
RUN chmod +x run.sh

USER runner
CMD ["./run.sh"]