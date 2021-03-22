import request from './request.js'

const EquipmentService = {};

EquipmentService.getEquipments = async () => {
    var endpoint = '/EquipmentIOT/all';
    var res = await request.get(endpoint,'');
    return res;
}

export default EquipmentService;