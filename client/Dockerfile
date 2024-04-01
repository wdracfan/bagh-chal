# pull official base image
FROM node:13.12.0-alpine

WORKDIR /app

# install app dependencies
COPY package.json /app
COPY package-lock.json /app
RUN npm install --silent
RUN npm install react-scripts@3.4.1 -g --silent

# add app
COPY . /app

# start app
ENTRYPOINT ["npm", "start"]