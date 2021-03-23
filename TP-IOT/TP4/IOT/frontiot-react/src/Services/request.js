import axios from 'axios';

//todo link api
let baseURL = "http://"+window.location.hostname+":8080";
const RequestManager = {};

function getHeader(){
    return {
      'Accept': 'application/json',
      'Content-Type': 'application/json'
    };
}

RequestManager.get =  async (ressource,object) => {
    var request = {};
    request.method = 'GET';
    request.headers = getHeader();
    //request.body = JSON.stringify(object);
    let res = await fetch(baseURL+''+ressource+""+object,request);
    if(!res.ok)throw new Error('request error'+res);
    else return await res.json();
};

RequestManager.post = async (ressource,object) => {
  var request = {};
  request.method = 'POST';
  request.headers = getHeader();
  request.body = JSON.stringify(object);
  let res = await fetch(baseURL+''+ressource, request);
  if(!res.ok) throw new Error('request error'+res);
  else return await res.json();
};

RequestManager.put = async (ressource,object) => {
  var request = {};
  request.method = 'PUT';
  request.headers = getHeader();
  request.body = JSON.stringify(object);
  let res = await fetch(baseURL+''+ressource, request);
  if(!res.ok) throw new Error('request error'+res);
  else return res;
};

RequestManager.delete = (ressource,object) => {
  axios.delete(baseURL+''+ressource, object)
    .then(res => { return res; })
    .catch((error) => { return error; })
};

export default RequestManager;