FROM python:3.13-bookworm

SHELL ["/bin/bash", "-c"]

RUN mkdir /app
WORKDIR /app

# install cli first, so it stays cached if we make changes to the tools
COPY ./rdpms-cli/               /app/rdpms-cli/
RUN pip install rdpms-cli/

# install tool dependencies
COPY ./plugins/tools/develop/requirements.txt   /app/plugins/tools/develop/requirements.txt
RUN pip install -r plugins/tools/develop/requirements.txt

COPY ./plugins/tools/develop/   /app/plugins/tools/develop/

RUN mkdir -p $HOME/.config/rdpms
RUN cp plugins/tools/develop/rdpms-cli-config.yaml $HOME/.config/rdpms/config.yaml

CMD ["plugins/tools/develop/workflow_rosbag_linear.sh", "workflow_rosbag_linear"]
