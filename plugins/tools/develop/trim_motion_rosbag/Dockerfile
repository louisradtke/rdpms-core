FROM alpine:latest AS downloader

RUN apk add curl unzip

RUN mkdir -p /work/download /work/extract /work/final
WORKDIR /work

RUN curl -fsSL https://github.com/AIT-Assistive-Autonomous-Systems/ros2bag_tools/archive/refs/heads/rolling.zip \
    -o download/ros2bag_tools.zip
RUN unzip download/ros2bag_tools.zip -d extract/

FROM ros:kilted

ARG DEBIAN_FRONTEND=noninteractive
SHELL ["/bin/bash", "-c"]

# install deps that seemed important
RUN --mount=type=cache,target=/var/cache/apt \
    --mount=type=cache,target=/var/lib/apt/lists \
    apt-get update \
    && apt-get install --no-install-recommends --quiet --yes \
      python3-pip \
      ros-dev-tools \
      python3-colcon-meson \
      python3-colcon-mixin \
      python3-importlib-metadata

RUN mkdir -p /ws/src

COPY --from=downloader /work/extract/* /ws/src/

WORKDIR /ws
RUN source /opt/ros/kilted/setup.bash && \
        apt-get update && \
        rosdep update && \
        rosdep install --from-paths src --ignore-src -r -y && \
        colcon build --merge-install

COPY ros_entrypoint.sh /ros_entrypoint.sh
RUN chmod +x /ros_entrypoint.sh

ENTRYPOINT ["/ros_entrypoint.sh"]
CMD ["bash"]
