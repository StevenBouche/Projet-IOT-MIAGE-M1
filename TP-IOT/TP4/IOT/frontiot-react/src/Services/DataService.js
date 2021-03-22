import request from './request.js'

const DataService = {};

DataService.getDataEquipment = async (id) => {
    var endpoint = '/DataIOT/equipment/'+id;
    var res = await request.get(endpoint,'');
    return res;
}

DataService.getLastData = async (id,nb) => {
    var endpoint = '/DataIOT/searchLast';
    var item = {
        EquipmentId: id,
        NbData: nb
    }
    var res = await request.post(endpoint,item);
    return res;
}

DataService.getLastDataUpdate = async (id,nb) => {
    var endpoint = '/DataIOT/search';
    var item = {
        EquipmentId: id,
        TimestampAfter: nb
    }
    var res = await request.post(endpoint,item);
    return res;
}

export default DataService;